using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class DrawModelScript : MonoBehaviour {
    public Renderer renderer;
    public Material cubeMat;
    //懒于计算识别图的四个顶点……所以要用找几个物体来标识这个位置
    public Transform[] marks;

    public Camera camera;
    private int frameCount;
    private bool isFixedTexture;

    

    private Vector2 _webcamTex2ScreenScale;

    // Use this for initialization
    void Start() {
        frameCount = 0;
        isFixedTexture = false;
    }

    // Update is called once per frame
    void Update() {
        if (!isFixedTexture) {
            //一开始BackgroundPlane的Texture并非摄像头的图像……等一会儿再设置。
            if (frameCount++ <= 30) return;
            isFixedTexture = true;
            var texture = renderer.material.mainTexture;
            cubeMat.mainTexture = texture;

            var maxScale = Mathf.Max((float)Screen.width / texture.width, (float)Screen.height / texture.height);

            var t2sWidth = texture.width * maxScale;
            var t2sHeight = texture.height * maxScale;

            _webcamTex2ScreenScale.x = Screen.width / t2sWidth;
            _webcamTex2ScreenScale.y = Screen.height / t2sHeight;
        }

        UpdateMatrix();

    }

    void UpdateMatrix() {
        var p0 = AdjustUV(camera.WorldToViewportPoint(marks[0].position));
        var p1 = AdjustUV(camera.WorldToViewportPoint(marks[1].position));
        var p2 = AdjustUV(camera.WorldToViewportPoint(marks[2].position));
        var p3 = AdjustUV(camera.WorldToViewportPoint(marks[3].position));

        cubeMat.SetVector("p0", p0);
        cubeMat.SetVector("p1", p1);
        cubeMat.SetVector("p2", p2);
        cubeMat.SetVector("p3", p3);
    }

    Vector3 AdjustUV(Vector3 v) {
        var webcamX = FixWebcamTextureToScreenUV(v.x, _webcamTex2ScreenScale.x);
        var webcamY = FixWebcamTextureToScreenUV(v.y, _webcamTex2ScreenScale.y);
        return new Vector3(webcamX * v.z, (1 - webcamY) * v.z, v.z);
    }

    float FixWebcamTextureToScreenUV(float value, float scale) {
        return (1.0f - scale) * 0.5f + value * scale;
    }
}