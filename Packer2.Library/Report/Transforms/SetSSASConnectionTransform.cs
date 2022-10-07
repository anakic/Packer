//using DataModelLoader.Report;
//using Newtonsoft.Json.Linq;

//namespace Packer2.Library.Report.Transforms
//{
//    public class SetSSASConnectionTransform : IReportTransform
//    {
//        private readonly string connectionString;

//        public SetSSASConnectionTransform(string connectionString)
//        {
//            this.connectionString = connectionString;
//        }

//        public PowerBIReport Transform(PowerBIReport model)
//        {
//            model.Connections.RemoveAll();

//            model.Connections.Add(
//                    new JProperty("Connections",
//                        new JArray(
//                                new JObject(
//                                        new JProperty("Name", "EntityDataSource"),
//                                        new JProperty("ConnectionString", connectionString),
//                                        new JProperty("ConnectionType", "analysisServicesDatabaseLive")
//                                )
//                        )
//                    )
//                );

//            return model;
//        }
//    }
//}
