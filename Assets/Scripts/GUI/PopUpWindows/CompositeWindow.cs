using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompositeWindow : MonoBehaviour {
    /*public enum PartType {
        CENTER,
        TOP, LEFT, RIGHT, BOTTOM,
        TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT
    }*/

    //public Dictionary<PartType, Sprite> windowParts;
    public Sprite center;
    public Sprite top, left, right, bottom;
    public Sprite topLeft, topRight, bottomLeft, BottomRight;
    public Sprite lightOnWindow;
    public int order = 0;
    public int sizeX;
    public int sizeY;

    public CompositeWindow() { }

    Vector2 centerPartSize;
    Vector2 size;

    void init() {
        if (center == null) print("Error! [init] windowParts == null");
        //Sprite center = windowParts[PartType.CENTER];
        //centerPartSize = new Vector2( center.rect.width, center.rect.height );
        
        float tWidth = topLeft.rect.width + topRight.textureRect.width + top.textureRect.width * sizeX;
        float tHeight = topLeft.rect.height + bottomLeft.textureRect.height + left.textureRect.height * sizeY;
        size = new Vector2(tWidth, tHeight);
        //print(size);
        Vector2 cursor = new Vector2( -tWidth, -tHeight ) * 0.005f;
        float startX = cursor.x;
        Transform parent = transform;

        Sprite curSprite = null;
        for (int y = 0; y <= sizeY+1; y++){
            for (int x = 0; x <= sizeX+1; x++) {
                if (x == 0) {
                    if (y == 0) curSprite = bottomLeft;
                    else if (y == sizeY + 1) curSprite = topLeft;
                    else curSprite = left;
                    cursor = new Vector2(startX, cursor.y + curSprite.rect.height * 0.01f);
                }
                else if (x == sizeX + 1) {
                    if (y == 0) curSprite = BottomRight;
                    else if (y == sizeY + 1) curSprite = topRight;
                    else curSprite = right;
                }
                
                else if (y == 0) curSprite = bottom;
                else if (y == sizeY + 1) curSprite = top;
                else curSprite = center;

                cursor = new Vector2(cursor.x + curSprite.rect.width * 0.01f, cursor.y);
                var go = new GameObject();
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = curSprite;
                sr.sortingOrder = order;
                go.transform.parent = parent;
                go.transform.localPosition = cursor - new Vector2(curSprite.rect.width * 0.005f, curSprite.rect.height * 0.005f);
            }
        }

        //float xToY = lightOnWindow.rect.width / lightOnWindow.rect.height;
        var lgo = new GameObject();
        var lsr = lgo.AddComponent<SpriteRenderer>();
        lsr.sprite = lightOnWindow;
        print(lsr.sprite.rect.width);
        float scaleX = lsr.sprite.rect.width / tWidth * 0.15f;
        float scaleY = lsr.sprite.rect.height / tHeight * 0.15f;
        print(scaleX +" "+ scaleY);
        lsr.sortingOrder = order+1;
        lgo.transform.parent = parent;
        lgo.transform.localPosition = new Vector2(0.0f, 0.0f);
        lgo.transform.localScale = new Vector3(scaleX * sizeX, scaleY * sizeY, 1.0f);
    }

    // Use this for initialization
    void Start () {
        init();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
