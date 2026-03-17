using UnityEngine;

public class KnightStatue : KnightBehavior
{
    protected override void Awake()
    {
        base.Awake();
        player = 3;
    }

    protected override void OnMouseDown()
    {
        //base.OnMouseDown();
        if (MapEditorData.instance.editMode && MapEditorData.instance.selectedObject == null)
        {
            currentSquare.knight = null;
            currentSquare = null;
            GetComponent<BoxCollider>().enabled = false;

            MapEditorData.instance.selectedObject = gameObject;

            ToggleGlow(true, 1);
        }
    }
}
