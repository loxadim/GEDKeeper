using System;
using System.IO;

namespace GedCom551
{
	public class TGEDCOMCustomRecord : TGEDCOMTag
	{
		protected string FXRef;

		public string XRef
		{
			get { return this.FXRef; }
			set {
				string oldXRef = this.FXRef;
				this.FXRef = value;
				if (this.FOwner != null) {
					this.FOwner.SetXRef(oldXRef, this);
				}
			}
		}

		protected override void SaveValueToStream(StreamWriter stream)
		{
			string S = base.Level.ToString();

			if (!string.IsNullOrEmpty(this.FXRef))
			{
				S = S + " " + "@" + this.FXRef + "@";
			}
			S = S + " " + base.Name;

			if (base.StringValue != "")
			{
				S = S + " " + base.StringValue;
			}

			stream.WriteLine(S);
		}

		public TGEDCOMCustomRecord(TGEDCOMTree owner, TGEDCOMObject parent, string tagName, string tagValue) : base(owner, parent, tagName, tagValue)
		{
		}
	}
}
