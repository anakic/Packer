let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_Spell = Source{[Schema="dbth",Item="Spell"]}[Data],
    #"Removed Columns" = Table.RemoveColumns(dbth_Spell,{"DateDischargeEstimated", "DateDischargePlanned", "DateReadyForDischarge"})
in
    #"Removed Columns"