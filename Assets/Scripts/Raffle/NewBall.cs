using UnityEngine;
using System.Collections;
using System;

public class NewBall : MonoBehaviour
{
    public event EventHandler OnBallStart;
    public event EventHandler OnBallStop;


    [SerializeField]
    private SpriteRenderer _spriteRView;
    [SerializeField]
    private SpriteRenderer _spriteRNumber1;
    [SerializeField]
    private SpriteRenderer _spriteRNumber2;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private CircleCollider2D _triggerArea;

    private

    void Awake()
    {

    }



    public void Initialize()
    {

    }

    public void SetColor(Color color)
    {
        _spriteRView.color = color;
    }
    public void SetNumber(int num)
    {
        // set number
    }

    void OnTriggerEnter2D(Collider2D other)
    {

    }
}



