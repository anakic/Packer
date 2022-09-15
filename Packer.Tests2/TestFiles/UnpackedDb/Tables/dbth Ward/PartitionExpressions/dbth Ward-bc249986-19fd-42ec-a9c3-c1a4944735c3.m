let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_Ward = Source{[Schema="dbth",Item="Ward"]}[Data]
in
    dbth_Ward