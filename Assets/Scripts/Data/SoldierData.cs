using UnityEngine;
using System;

[Serializable] public class SoldierData {
    public Sprite portrait;
    public string firstName;
    public string lastName;
    public string bio;
    public string serialNumber;
    public SoldierRoleData roleData;
    public int currentHealth;
}