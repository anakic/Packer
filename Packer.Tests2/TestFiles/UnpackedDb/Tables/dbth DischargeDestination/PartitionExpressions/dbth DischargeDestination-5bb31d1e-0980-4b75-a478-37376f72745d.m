let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_DischargeDestination = Source{[Schema="dbth",Item="DischargeDestination"]}[Data]
in
    dbth_DischargeDestination