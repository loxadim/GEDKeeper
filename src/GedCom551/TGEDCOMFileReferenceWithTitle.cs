using System;

namespace GedCom551
{
	public sealed class TGEDCOMFileReferenceWithTitle : TGEDCOMFileReference
	{
		public string Title
		{
			get { return base.GetTagStringValue("TITL"); }
			set { base.SetTagStringValue("TITL", value); }
		}

		protected override string MediaTypeTagName()
		{
			return "FORM\\TYPE";
		}

		public TGEDCOMFileReferenceWithTitle(TGEDCOMTree owner, TGEDCOMObject parent, string tagName, string tagValue) : base(owner, parent, tagName, tagValue)
		{
		}
	}
}
