let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_AttendanceCategory = Source{[Schema="dbth",Item="AttendanceCategory"]}[Data]
in
    dbth_AttendanceCategory