using System;
using System.Collections.Generic;

[Serializable] public class TownPosition {
    public float x;
    public float y;
}

[Serializable] public class Town {
    public string name;
    public TownPosition position;
}

[Serializable] public class TownList {
    public List<Town> towns;
}