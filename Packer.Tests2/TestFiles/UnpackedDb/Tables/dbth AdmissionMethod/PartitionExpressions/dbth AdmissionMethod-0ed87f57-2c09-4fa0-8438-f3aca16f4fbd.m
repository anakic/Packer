let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_AdmissionMethod = Source{[Schema="dbth",Item="AdmissionMethod"]}[Data]
in
    dbth_AdmissionMethod