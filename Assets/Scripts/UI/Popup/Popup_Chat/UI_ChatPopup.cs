using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatPopup : UI_Popup
{
    public GameObject ChatManager;
    ChatManager chatManager;

    public RoomImage roomImage;
    public string roomName;

    GameObject mediaPanel;
    OpenFileDialog openFileDialog;
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

        if (roomImage != null)
            roomName = roomImage.roomName;

        chatManager = ChatManager.GetComponent<ChatManager>();

        BindInputField(typeof(TMP_InputField));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.Send).gameObject.BindEvent(send);
        GetButton((int)Buttons.Media).gameObject.BindEvent(media);
        GetButton((int)Buttons.Close).gameObject.BindEvent(close);

        return true;
    }

    public void send()
    {
        if (TCPClient.instance.ImageLoaded)
        {
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
        if (mediaPanel != null)
            Destroy(mediaPanel);

        mediaPanel = Managers.Resource.Instantiate("UI/inChatPopup/ImageLoadPanel", transform);
        Image image = mediaPanel.transform.Find("Image").GetComponent<Image>();

        accessToFile(image);

        if (image != null && !string.IsNullOrEmpty(TCPClient.instance.LoadedImage))
            TCPClient.instance.ImageLoaded = true;
    }

    public void close()
    {
        TCPClient.instance.SendLeaveRoom(roomName);
        DestroyImmediate(gameObject);
    }

    #region 이미지 전송 관련 코드

    public void accessToFile(Image image)
    {
        openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*",
            FilterIndex = 1,
            Title = "이미지 파일 선택"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string fileName = openFileDialog.FileName;
            TCPClient.instance.LoadedImage = fileName;

            byteTexture = File.ReadAllBytes(fileName);
            MakeImageFromBytes(image, byteTexture);
        }
        else
        {
            Destroy(mediaPanel); // 파일 선택이 취소되었을 경우 미디어 패널 제거
        }
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
