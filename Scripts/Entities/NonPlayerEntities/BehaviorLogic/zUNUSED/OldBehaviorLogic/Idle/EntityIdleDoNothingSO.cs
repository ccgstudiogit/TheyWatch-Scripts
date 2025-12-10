using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Idle Logic/Do Nothing")]
public class EntityIdleDoNothingSO : EntityIdleSOBase
{
    [SerializeField] private bool makeEntityDoNothing = true;

    public override void DoEnterLogic()
    {
        if (makeEntityDoNothing && entity is MonoBehaviour monoBehaviour)
            monoBehaviour.enabled = false;
    }
}
