using UnityEngine.UI;

    public static class GraphicExtenions
    {
        public static void SetAlpha(this MaskableGraphic image, float a)
        {
            var color = image.color;
            color.a = a;
            image.color = color;
        }
    }