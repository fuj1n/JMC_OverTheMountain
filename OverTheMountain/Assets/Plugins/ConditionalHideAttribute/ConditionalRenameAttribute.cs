using System;
using UnityEngine;

//Derived from ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: - 
//Arthur Uzulin - 18/10/2018

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalRenameAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public string ConditionalSourceField2 = "";
    public string[] ConditionalSourceFields = new string[] { };
    public bool[] ConditionalSourceFieldInverseBools = new bool[] { };
    public bool Inverse = false;
    public bool UseOrLogic = false;

    public bool InverseCondition1 = false;
    public bool InverseCondition2 = false;

    public string Name = "";
}



