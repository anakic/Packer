let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_HospitalSite = Source{[Schema="dbth",Item="HospitalSite"]}[Data]
in
    dbth_HospitalSite