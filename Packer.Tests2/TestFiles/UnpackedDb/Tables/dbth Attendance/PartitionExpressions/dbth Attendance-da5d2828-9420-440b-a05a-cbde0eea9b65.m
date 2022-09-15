let
    Source = Sql.Database("20.117.159.235", "Dataflow"),
    dbth_Attendance = Source{[Schema="dbth",Item="Attendance"]}[Data]
in
    dbth_Attendance