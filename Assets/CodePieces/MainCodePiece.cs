public class MainCodePiece : CodePiece
{
    #region Overrides

    public override void AttachTo(CodeSlot slot)
    {
        //Don't attach
    }

    public override void DetachFromParent()
    {
        //Don't detach
    }

    protected override void InitElements()
    {
        //No bottom slot :)
    }

    protected override void InitVisuals()
    {
        base.InitVisuals();
        m_Visuals.isSeparatePiece = true;
    }

    protected override void Start()
    {
        base.Start();
        parentCode = this;
    }

    #endregion
}
