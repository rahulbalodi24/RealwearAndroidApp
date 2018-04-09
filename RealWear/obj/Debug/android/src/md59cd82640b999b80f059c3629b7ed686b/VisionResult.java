package md59cd82640b999b80f059c3629b7ed686b;


public class VisionResult
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;Landroid/os/PersistableBundle;)V:GetOnCreate_Landroid_os_Bundle_Landroid_os_PersistableBundle_Handler\n" +
			"";
		mono.android.Runtime.register ("RealWear.VisionResult, RealWear, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", VisionResult.class, __md_methods);
	}


	public VisionResult ()
	{
		super ();
		if (getClass () == VisionResult.class)
			mono.android.TypeManager.Activate ("RealWear.VisionResult, RealWear, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0, android.os.PersistableBundle p1)
	{
		n_onCreate (p0, p1);
	}

	private native void n_onCreate (android.os.Bundle p0, android.os.PersistableBundle p1);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
