using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Windows.Forms;
using Ookii.Dialogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using UnityEngine.UI;

public class ImageLoadtest : MonoBehaviour
{
    VistaOpenFileDialog OpenDialog;
    Stream openStream;
    public Image image;

    public void Start()
    {
        OpenDialog = new VistaOpenFileDialog();
        OpenDialog.Filter = "jpg files (*.jpg) |*.jpg|png files (*.png) |*.jpg|All files  (*.*)|*.*";
        OpenDialog.FilterIndex = 3;
        OpenDialog.Title = "파일 첨부";
    }

    public string FileOpen()
    {
        if(OpenDialog.ShowDialog() == DialogResult.OK)
        {
            if((openStream = OpenDialog.OpenFile()) != null)
            {
                openStream.Close();
                return OpenDialog.FileName;
            }
        }
        return null;
    }

    public void accessToFile()
    {
        string fileName = FileOpen();
        byte[] byteTexture = System.IO.File.ReadAllBytes(fileName);

        if (!string.IsNullOrEmpty(fileName))
        {
            Debug.Log(fileName);
            loadImgFromPath(image, byteTexture);
        }
    }

    void loadImgFromPath(Image image, byte[] byteTexture)
    {
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(byteTexture);

        Rect rect = new Rect(0, 0, texture.width, texture.height);

        image.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }

    //public void OnGUI()
    //{
    //    if (GUI.Button(new Rect(100, 100, 50, 50), "이미지"))
    //    {
    //        string fileName = FileOpen();

    //        if (!string.IsNullOrEmpty(fileName))
    //        {
    //            Debug.Log(fileName);
    //        }
    //    }
    //}
}
