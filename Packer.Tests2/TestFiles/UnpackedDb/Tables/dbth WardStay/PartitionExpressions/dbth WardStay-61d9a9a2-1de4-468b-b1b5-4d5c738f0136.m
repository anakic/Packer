let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_WardStay = Source{[Schema="dbth",Item="WardStay"]}[Data]
in
    dbth_WardStay