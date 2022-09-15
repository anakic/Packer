let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_SourceOfAdmission = Source{[Schema="dbth",Item="SourceOfAdmission"]}[Data]
in
    dbth_SourceOfAdmission