let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_Episode = Source{[Schema="dbth",Item="Episode"]}[Data]
in
    dbth_Episode