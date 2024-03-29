﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class SimplifyBookmarks : IJObjTransform
    {
        private ILogger logger;

        public SimplifyBookmarks(ILogger logger)
        {
            this.logger = logger;
        }

        public void Restore(JObject obj)
        {
            // restore nothing - that useless fluff is toast!
        }

        public void Transform(JObject obj)
        {
            List<JObject> bookmarkJobjs = new List<JObject>();
            foreach (var jo in obj.SelectTokens(".#config.bookmarks[*]").OfType<JObject>())
            {
                var children = jo["children"] as JArray;
                if (children == null)
                    bookmarkJobjs.Add(jo);
                else
                    bookmarkJobjs.AddRange(children.OfType<JObject>());
            }

            foreach (var bookmarkJObj in bookmarkJobjs)
            {
                logger.LogInformation("Simplifying bookmark at path '{path}'.", bookmarkJObj.Path);

                var applyOnlyToTargetVisuals = bookmarkJObj.SelectToken("options.applyOnlyToTargetVisuals")?.Value<bool>() ?? false;

                var targetVisualsArr = (JArray)bookmarkJObj.SelectToken("options.targetVisualNames")!;
                var targetVisualNames = targetVisualsArr.ToObject<string[]>()!.ToHashSet();
                var targetVisualNamesFound = new HashSet<string>();
                var containers1 = ((JObject)bookmarkJObj.SelectToken("explorationState.sections..visualContainers")!).Properties();
                var containers2 = bookmarkJObj.SelectTokens("explorationState.sections..visualContainerGroups..children")!.SelectMany(t => ((JObject)t).Properties());


                // clear data first(we might get rid of entire element if it's left empty after this)
                var suppressData = bookmarkJObj.SelectToken("options.suppressData")?.Value<bool>() ?? false;
                if (suppressData)
                {

                    var nodesToRemove1 = bookmarkJObj.SelectTokens("explorationState..filters").ToList();
                    var nodesToRemove2 = bookmarkJObj.SelectTokens("explorationState..visualContainers..singleVisual.activeProjections").ToList();
                    var nodesToRemove3 = bookmarkJObj.SelectTokens("explorationState..visualContainers..singleVisual.orderBy").ToList();
                    nodesToRemove1
                        .Union(nodesToRemove2)
                        .Union(nodesToRemove3)
                        .ToList()
                        .ForEach(x => x.Parent.Remove());
                }

                foreach (JProperty c in containers1.Union(containers2).ToList())
                {
                    bool removed = false;

                    // remove the visual's node if not in targetVisuals and applyOnlyToTargetVisuals=true
                    if (applyOnlyToTargetVisuals && !targetVisualNames.Contains(c.Name))
                    {

                        c.Remove();
                        removed = true;
                    }

                    //// commenting this out for now, it seems like this step does mess up the behavior of bookmarks 
                    //// when opened in power bi, not sure why.
                    //if (!removed)
                    //{
                    //    // remove the visual's node if no useful data inside it
                    //    var singleVisualNode = (JObject)c.Value["singleVisual"]!;
                    //    if (singleVisualNode != null)
                    //    {
                    //        if (singleVisualNode.Properties().Count(p => new[] { "visualType", "objects" }.Contains(p.Name) == false) == 0)
                    //        {
                    //            var objectsSubNode = (JObject)singleVisualNode["objects"]!;
                    //            if (objectsSubNode == null || objectsSubNode.Properties().Count() == 0)
                    //            {
                    //                c.Remove();
                    //                removed = true;
                    //            }
                    //        }
                    //    }
                    //}

                    if (!removed)
                        targetVisualNamesFound.Add(c.Name);
                }

                //targetVisualsArr.Replace(new JArray(targetVisualNamesFound));
            }
        }
    }
}
