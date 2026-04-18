using UnityEngine;

public class SimpleWorkerLimbSwing : MonoBehaviour
{
    [SerializeField] private Transform armL;
    [SerializeField] private Transform armR;
    [SerializeField] private Transform legL;
    [SerializeField] private Transform legR;

    [SerializeField] private float armSwingAngle = 18f;
    [SerializeField] private float legSwingAngle = 16f;
    [SerializeField] private float swingSpeed = 8f;

    private Quaternion armLBaseRot;
    private Quaternion armRBaseRot;
    private Quaternion legLBaseRot;
    private Quaternion legRBaseRot;

    private bool isMoving;

    private void Start()
    {
        if (armL != null) armLBaseRot = armL.localRotation;
        if (armR != null) armRBaseRot = armR.localRotation;
        if (legL != null) legLBaseRot = legL.localRotation;
        if (legR != null) legRBaseRot = legR.localRotation;
    }

    private void Update()
    {
        float t = isMoving ? Mathf.Sin(Time.time * swingSpeed) : 0f;

        float armSwing = t * armSwingAngle;
        float legSwing = t * legSwingAngle;

        if (armL != null)
            armL.localRotation = armLBaseRot * Quaternion.Euler(armSwing, 0f, 0f);

        if (armR != null)
            armR.localRotation = armRBaseRot * Quaternion.Euler(-armSwing, 0f, 0f);

        if (legL != null)
            legL.localRotation = legLBaseRot * Quaternion.Euler(-legSwing, 0f, 0f);

        if (legR != null)
            legR.localRotation = legRBaseRot * Quaternion.Euler(legSwing, 0f, 0f);
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }
}