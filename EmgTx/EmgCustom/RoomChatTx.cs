using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 用于在房间内添加对话脚本，本质属于RoomSpecificScripts
/// </summary>
public class RoomChatTx : UpdatableAndDeletable
{
    /// <summary>
    /// 辅助判断当前语种是否需要额外延长等待时间。在 AddEvent 时可以直接将 extralinger 乘以该数值
    /// </summary>
    public int extraLingerFactor = 0;

    public int age;
    public int preWaitCounter;
    public List<TextEvent> events = new List<TextEvent>();
    public Player player;

    public bool textPrompted = false;
    public bool inited = false;

    public string textPrompt;
    public int textPromptWait;
    public int textPromptTime;

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="room"></param>
    /// <param name="preWaitCounter">在正式开始对话前等待多少逻辑帧</param>
    /// <param name="textprompt">是否需要额外的 textPrompt 效果。空字符默认为没有，首先会等待 textPrompt 加载完成，然后再进入正常的逻辑循环</param>
    /// <param name="textPromptWait">textPrompt 效果的等待时间</param>
    /// <param name="textPromptTime">textPrompt 效果的持续时间</param>
    public RoomChatTx(Room room, int preWaitCounter, string textprompt = "",int textPromptWait = 0,int textPromptTime = 320)
    {
        base.room = room;
        this.preWaitCounter = preWaitCounter;

        this.textPrompt = textprompt;
        this.textPromptWait = textPromptWait;
        this.textPromptTime = textPromptTime;

        extraLingerFactor = room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;

        if (textPrompt == string.Empty)
            textPrompted = true;
    }

    /// <summary>
    /// 调用游戏内的翻译方法。
    /// </summary>
    /// <param name="orig"></param>
    /// <returns></returns>
    public string Translate(string orig)
    {
        string result = room.game.rainWorld.inGameTranslator.Translate(orig);
        EmgTxCustom.Log("{0}\n{1}", orig, result);
        return result;
    }

    void PromptText()
    {
        if (textPrompted)
            return;
        if (room.game.cameras == null || room.game.cameras[0].hud == null)
            return;
        if (room.game.cameras[0].hud.textPrompt == null)
            room.game.cameras[0].hud.AddPart(new TextPrompt(room.game.cameras[0].hud));

        room.game.cameras[0].hud.textPrompt.AddMessage(Translate(textPrompt), textPromptWait, textPromptTime, true, true);
        textPrompted = true;
    }

    void SetUp()
    {
        if (room.game.cameras == null || room.game.cameras[0].hud == null)
        {
            EmgTxCustom.Log("Set up failure : {0},{1}", room.game.cameras, room.game.cameras[0].hud);
            return;
        }

        for (int i = 0; i < room.game.cameras.Length; i++)
        {
            if (room.game.cameras[i].hud != null && room.game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && room.game.cameras[i].followAbstractCreature.Room == room.abstractRoom)
            {
                if (room.game.cameras[i].hud.dialogBox == null)
                {
                    room.game.cameras[i].hud.InitDialogBox();
                }
            }
        }


        var dialogBox = room.game.cameras[0].hud.dialogBox;
        AddTextEvents(dialogBox);

        EmgTxCustom.Log("text events inited! {0}", events.Count);
    }

    /// <summary>
    /// 添加对话事件，需要手动完成实现
    /// </summary>
    /// <param name="dialogBox"></param>
    public virtual void AddTextEvents(DialogBox dialogBox)
    {
        inited = true;
    }

    public override void Update(bool eu)
    {
        if (slatedForDeletetion)
            return;
        base.Update(eu);

        if (!textPrompted)
        {
            PromptText();
            return;
        }

        if (!inited)
        {
            if (room.game.cameras[0].hud.textPrompt.currentlyShowing == TextPrompt.InfoID.Nothing)
                SetUp();
            return;
        }

        AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
        if (firstAlivePlayer == null)
        {
            return;
        }
        if (player == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
        {
            player = (firstAlivePlayer.realizedCreature as Player);
        }

        age++;
        if (age > preWaitCounter)
        {
            if (events.Count == 0)
            {
                Destroy();
                return;
            }
            var current = events[0];
            current.Update();
            if (current.IsOver)
                events.RemoveAt(0);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    /// <summary>
    /// 文本事件类
    /// </summary>
    public class TextEvent
    {
        public RoomChatTx owner;

        public string text;
        public DialogBox dialogBox;
        public int age;
        public int initWait;
        public int extraLinger;
        public bool activated = false;

        public SoundID partWithSound;
        bool loop;
        float vol;
        float pitch;

        public bool IsOver
        {
            get
            {
                if (age < initWait || !activated)
                    return false;
                return dialogBox.CurrentMessage == null;
            }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="dialogBox"></param>
        /// <param name="initWait">开始对话前的等待时间</param>
        /// <param name="extraLinger">额外延长的等待时间</param>
        /// <param name="partWithSound">伴随对话播放的声音id</param>
        /// <param name="loop">声音是否循环，若id为空则该项设置无效</param>
        /// <param name="vol">声音音量，若id为空则该项设置无效</param>
        /// <param name="pitch">声音声调，，若id为空则该项设置无效</param>
        public TextEvent(RoomChatTx owner, string text, DialogBox dialogBox, int initWait, int extraLinger = 40, SoundID partWithSound = null, bool loop = false,float vol = 1f,float pitch = 1f)
        {
            this.owner = owner;
            this.text = text;
            this.dialogBox = dialogBox;
            this.initWait = initWait;
            this.partWithSound = partWithSound;
            this.extraLinger = extraLinger;

            this.loop = loop;
            this.pitch = pitch;
            this.vol = vol;
        }

        public void Activate()
        {
            activated = true;
            if (text != "")
                dialogBox.NewMessage(text, extraLinger);
            if (partWithSound != null)
                owner.room.PlaySound(partWithSound, owner.player.mainBodyChunk, loop, vol, pitch);

            EmgTxCustom.Log("New message {0}", text);
        }

        public void Update()
        {
            if (!activated && age >= initWait)
                Activate();
            age++;
        }
    }

    /// <summary>
    /// 等待事件
    /// </summary>
    public class PauseEvent : TextEvent
    {
        public PauseEvent(RoomChatTx owner,DialogBox dialogBox,int initWait,int extraLinger = 40) : base(owner, "", dialogBox, initWait, extraLinger)
        {
        }
    }
}
