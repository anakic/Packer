let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_MainSpecialty = Source{[Schema="dbth",Item="MainSpecialty"]}[Data]
in
    dbth_MainSpecialty