using UnityEngine;
using System.Collections;
// Эффект вращения цели с указанной скоростью
public class Rotating : MonoBehaviour{
    public float m_rotateVelocity;
    public static Rotating set(GameObject target, float rotateVelocity) // указывается цель которая будет вращаться и скорость вращения
    {
        Rotating rotating = target.AddComponent<Rotating>();
        rotating.m_rotateVelocity = rotateVelocity;
        return rotating;
    }
    float slowdownToSpeed = -1, slowdownBrakingRate; // дополнительные возможности, установить к эфекту затухания скорости, с определённой силой торможения и до определённой скорости.
    public void slowdown(float toSpeed, float brakingRate) // установка дополнительных возможностей
    {
        slowdownToSpeed = toSpeed; slowdownBrakingRate = brakingRate;
    }
    void Start () {}
	void Update () {
        if (slowdownToSpeed != -1 && m_rotateVelocity > slowdownToSpeed)
            m_rotateVelocity *= slowdownBrakingRate;
        transform.Rotate( new Vector3(0.0f, 0.0f, m_rotateVelocity) );
    }
}
