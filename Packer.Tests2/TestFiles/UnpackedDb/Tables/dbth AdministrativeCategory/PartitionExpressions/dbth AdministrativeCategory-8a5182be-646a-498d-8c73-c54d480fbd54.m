let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_AdministrativeCategory = Source{[Schema="dbth",Item="AdministrativeCategory"]}[Data]
in
    dbth_AdministrativeCategory