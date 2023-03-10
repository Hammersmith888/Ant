using UnityEngine;


public class HPBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sr;

    public void Set(float value)
    {
        gameObject.SetActive(true);

        Sprite sprite = _sr.sprite;
        float width = sprite.texture.width / sprite.pixelsPerUnit * value;
        _sr.size = _sr.size.SetX(width);
    }
}