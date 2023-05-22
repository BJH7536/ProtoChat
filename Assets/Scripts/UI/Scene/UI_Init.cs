using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UI_Init : UI_Scene
{
    // TODO
    // 여기서 라디오버튼으로
    // Role과 Protocol을 정해서 submit버튼을 누르면
    // 다음으로 넘어가는데,
    // 여기서 정한 거에 따라 다른 Scene으로 넘어가게끔.

    // 1. Server을 정했으면
    // 넘어감과 동시에 port를 입력받아서 서버를 열게끔

    // 2. Client를 정했으면
    // 넘어갈 때 서버 IP와 Port를 입력받아서 해당 서버를 연결 할 수 있게끔


    enum Buttons
    {

    }

    private void Start()
    {
        

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;

    }


}