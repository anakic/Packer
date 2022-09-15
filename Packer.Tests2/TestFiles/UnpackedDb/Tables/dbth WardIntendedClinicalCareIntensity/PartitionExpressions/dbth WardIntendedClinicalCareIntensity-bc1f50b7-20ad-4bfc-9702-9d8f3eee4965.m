let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_WardIntendedClinicalCareIntensity = Source{[Schema="dbth",Item="WardIntendedClinicalCareIntensity"]}[Data]
in
    dbth_WardIntendedClinicalCareIntensity