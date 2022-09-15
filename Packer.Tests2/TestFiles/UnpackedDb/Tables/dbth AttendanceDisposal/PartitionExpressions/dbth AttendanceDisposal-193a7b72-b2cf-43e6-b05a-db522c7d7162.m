let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_AttendanceDisposal = Source{[Schema="dbth",Item="AttendanceDisposal"]}[Data]
in
    dbth_AttendanceDisposal