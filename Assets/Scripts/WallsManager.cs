using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsManager : MonoBehaviour
{
    public MultipleMaterials materials;
    private Renderer rendererComponent;

    private bool isDefault = true;

    private void Awake() {
        rendererComponent = GetComponent<Renderer>();
    }

    public void Hide(){
        Material transparentWall = materials.secondaryMaterials[0];
        rendererComponent.material = transparentWall;
    }

    public void Show(){
        rendererComponent.material = materials.defaultMaterial;
    }

}
