using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class DowngradeTransform : IDataModelTransform
    {
        private readonly ILogger<DowngradeTransform> logger;
        private readonly int targetCompatibilityLevel;

        #region reflection to load compatibility levels for types/properties
        class PropertyCompatibilityLevels
        {
            public PropertyCompatibilityLevels(PropertyInfo property, string level)
            {
                Property = property;
                LevelString = level;

                if (level != null && Regex.IsMatch(level, @"\d+"))
                    LevelNum = int.Parse(level);
            }

            public PropertyInfo Property { get; }

            public string LevelString { get; }

            public int? LevelNum { get; }
        }

        static Lazy<List<PropertyCompatibilityLevels>> compatibilityLevels;

        static DowngradeTransform()
        {
            compatibilityLevels = new Lazy<List<PropertyCompatibilityLevels>>(() =>
            {
                return typeof(Database).Assembly.ExportedTypes.SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    .Select(pi =>
                    {
                        string level = null;
                        try
                        {
                            var att = pi.GetCustomAttributes().SingleOrDefault(a => a.GetType().Name == "CompatibilityRequirementAttribute");
                            if (att != null)
                                level = (string)att.GetType().GetProperties()[0].GetValue(att, null);
                        }
                        catch { }
                        return new PropertyCompatibilityLevels(pi, level);
                    })
                    .Where(x => x.LevelString != null)
                    .ToList();
            });
        }
        #endregion

        public DowngradeTransform(int targetCompatibilityLevel, ILogger<DowngradeTransform> logger = null)
        {
            this.logger = logger ?? new DummyLogger<DowngradeTransform>();
            this.targetCompatibilityLevel = targetCompatibilityLevel;
        }

        public Database Transform(Database database)
        {
            // todo #1: we currently indiscriminantly remove all properties that appear in the list (by name)
            // without making a distinction between the properties that have the same name but live on different 
            // types and have different compatibility levels. Currently we're ignoring this possibility because it 
            // can be tricky to find the type that a json object corresponds to, but it can be done (and probably 
            // should to make this solution robust). This is just a first version that seems to work fine at the moment,
            // but revisiting this and implementing it properly might be a good idea as there is potential for this
            // shortcut to bite us later on, as this logic could remove valid properties because a new property
            // with the same name and a higher compatibility level gets added to some TOM type later on.
            //
            // Note: since we have the database as as instance in memory AND we have the JSON, it would probably be fairly
            // straight forward to map each jobject to a TOM object, so we shouldn't have too much trouble mapping
            // JProperties to .NET object properties. To help with performance, we could do this only for cases where
            // the property exists with the same name on two types and one of the levels is compatible and one isn't.

            // todo #2: Enum values also have [CompatibilityRequirement] attributes, e.g. ModeType. And types can have
            // them as well, e.g. DataSourceOptions. We must make sure to at least warn about this. Come to think of it,
            // some sort of UI for conversion would probably be ideal as we can't just "fix" these things automatically
            // without asking the user what to do. Removing the offending properties and values might lose important logic.

            // gnerate list of properties that are not compatible with the target compatibility level
            var incompatibleProperties = compatibilityLevels.Value
                .Where(x => !x.LevelNum.HasValue /*preview, unsupported*/ || x.LevelNum > targetCompatibilityLevel)
                .Select(inf => inf.Property.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // generate json object based on current model model
            var file = new MemoryFile();
            var bimStore = new BimDataModelStore(file);
            bimStore.Save(database);
            var jobj = JObject.Parse(file.Text);

            // this property is invalid, but I didn't find it as a property in the AS dll
            // todo: instead of the black list of incompatible properties, should we come up with a white list instead?
            // this would eliminate the defaultPowerBIDataSourceVersion property as well
            var aForbiddenToken = jobj.SelectToken(".model.defaultPowerBIDataSourceVersion");
            if (aForbiddenToken != null)
            {
                aForbiddenToken.Parent!.Remove();
                logger.LogInformation("Removed model.defaultPowerBIDataSourceVersion property");
            }

            // remove imcompatible properties
            foreach (var jProp in jobj.DescendantsAndSelf().OfType<JProperty>().ToArray())
            {
                if (incompatibleProperties.Contains(jProp.Name))
                {
                    jProp.Remove();
                    logger.LogTrace("Removed incompatible property {propertyName} from path {jPath}", jProp.Name, jProp.Path);
                }
            }

            // materialize new model from edited json object
            file.SetText(jobj.ToString());
            database = bimStore.Read();

            // change the compatibility to desired level (should work after removing incompatible properties)
            database.CompatibilityLevel = targetCompatibilityLevel;

            return database;
        }
    }
}
