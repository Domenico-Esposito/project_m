using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName="Multiple Materials")]
public class MultipleMaterials : ScriptableObject
{
    public Material defaultMaterial;
    public Material[] secondaryMaterials;

}
