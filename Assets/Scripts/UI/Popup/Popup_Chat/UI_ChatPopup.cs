using Ookii.Dialogs;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatPopup : UI_Popup
{
    //public Vector3 initPos;
    public GameObject ChatManager;
    ChatManager chatManager;

    public RoomImage roomImage;
    public string roomName;

    GameObject mediaPanel;
    VistaOpenFileDialog OpenDialog;
    Stream openStream;

    byte[] byteTexture;

    enum TMP_InputField
    {
        ChatInputField,
    }

    enum Buttons
    {
        Send,
        Media,
        Close,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //initPos = gameObject.GetComponent<RectTransform>().anchoredPosition;
        //Debug.Log(initPos);

        if(roomImage != null)
            roomName = roomImage.roomName;

        chatManager = ChatManager.GetComponent<ChatManager>();
        setDialog();

        BindInputField(typeof(TMP_InputField));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.Send).gameObject.BindEvent(send);
        GetButton((int)Buttons.Media).gameObject.BindEvent(media);
        GetButton((int)Buttons.Close).gameObject.BindEvent(close);

        return true;

    }

    public void send()
    {
        if(TCPClient.instance.ImageLoaded)
        {
            // TODO
            // 이미지 전송
            TCPClient.instance.SendImage(roomName, byteTexture);

            Destroy(mediaPanel);
            TCPClient.instance.ImageLoaded = false;
        }
        else
        {
            string text = GetInputField((int)TMP_InputField.ChatInputField).text.Trim();

            if (text == "")
                return;
            GetInputField((int)TMP_InputField.ChatInputField).text = "";
            TCPClient.instance.SendChatMessage(roomName, text);
        }
    }

    public void media()
    {
        if(mediaPanel != null)
            Destroy(mediaPanel);
        mediaPanel = Managers.Resource.Instantiate("UI/inChatPopup/ImageLoadPanel", transform);
        Image image = mediaPanel.transform.Find("Image").GetComponent<Image>();

        accessToFile(image);

        if (image != null)
            TCPClient.instance.ImageLoaded = true;
    }

    public void close()
    {
        TCPClient.instance.SendLeaveRoom(roomName);
        DestroyImmediate(gameObject);
    }

    #region 이미지 전송 관련 코드
    public void setDialog()
    {
        OpenDialog = new VistaOpenFileDialog();
        OpenDialog.Filter = "jpg files (*.jpg) |*.jpg|png files (*.png) |*.jpg|All files  (*.*)|*.*";
        OpenDialog.FilterIndex = 3;
        OpenDialog.Title = "파일 첨부";
    }

    public void accessToFile(Image image)
    {
        string fileName = FileOpen();

        TCPClient.instance.LoadedImage = fileName;

        byteTexture = System.IO.File.ReadAllBytes(fileName);

        if (!string.IsNullOrEmpty(fileName))
        {
            MakeImageFromBytes(image, byteTexture);
        }

    }

    public string FileOpen()
    {
        if (OpenDialog.ShowDialog() == DialogResult.OK)
        {
            if ((openStream = OpenDialog.OpenFile()) != null)
            {
                openStream.Close();
                return OpenDialog.FileName;
            }
        }
        return null;
    }

    void MakeImageFromBytes(Image image, byte[] byteTexture)
    {
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(byteTexture);

        Rect rect = new Rect(0, 0, texture.width, texture.height);

        image.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }

    #endregion 이미지 전송 관련 코드
}