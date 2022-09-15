let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_WardIntendedAgeGroup = Source{[Schema="dbth",Item="WardIntendedAgeGroup"]}[Data]
in
    dbth_WardIntendedAgeGroup