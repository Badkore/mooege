﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3Sharp.Net.Game.Messages.Animation
{
    public class PlayAnimationMessage : GameMessage
    {
        public int Field0;
        public int Field1;
        public float Field2;
        // MaxLength = 3
        public PlayAnimationMessageSpec[] tAnim;

        public override void Handle(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void Parse(GameBitBuffer buffer)
        {
            Field0 = buffer.ReadInt(32);
            Field1 = buffer.ReadInt(4);
            Field2 = buffer.ReadFloat32();
            tAnim = new PlayAnimationMessageSpec[buffer.ReadInt(2)];
            for (int i = 0; i < tAnim.Length; i++) { tAnim[i] = new PlayAnimationMessageSpec(); tAnim[i].Parse(buffer); }
        }

        public override void Encode(GameBitBuffer buffer)
        {
            buffer.WriteInt(32, Field0);
            buffer.WriteInt(4, Field1);
            buffer.WriteFloat32(Field2);
            buffer.WriteInt(2, tAnim.Length);
            for (int i = 0; i < tAnim.Length; i++) { tAnim[i].Encode(buffer); }
        }

        public override void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("PlayAnimationMessage:");
            b.Append(' ', pad++);
            b.AppendLine("{");
            b.Append(' ', pad); b.AppendLine("Field0: 0x" + Field0.ToString("X8") + " (" + Field0 + ")");
            b.Append(' ', pad); b.AppendLine("Field1: 0x" + Field1.ToString("X8") + " (" + Field1 + ")");
            b.Append(' ', pad); b.AppendLine("Field2: " + Field2.ToString("G"));
            b.Append(' ', pad); b.AppendLine("tAnim:");
            b.Append(' ', pad); b.AppendLine("{");
            for (int i = 0; i < tAnim.Length; i++) { tAnim[i].AsText(b, pad + 1); b.AppendLine(); }
            b.Append(' ', pad); b.AppendLine("}"); b.AppendLine();
            b.Append(' ', --pad);
            b.AppendLine("}");
        }
    }
}