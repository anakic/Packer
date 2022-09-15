let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_DischargeMethod = Source{[Schema="dbth",Item="DischargeMethod"]}[Data]
in
    dbth_DischargeMethod