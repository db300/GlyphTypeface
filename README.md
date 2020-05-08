# GlyphTypeface
基于 GlyphTypeface 的字体管理类库，目前主要用于字形字符串渲染

### 引用

`iHawkGlyphTypefaceLibrary`

### Demo：

    var gtm = new iHawkGlyphTypefaceLibrary.GlyphTypefaceManager(<font file name>);
	var ss = new List<string>
    {
    	"这是一个字符串",
		"the quick brown fox jumps over the lazy dog",
		"0123456789",
		"永"
	};
	foreach (var s in ss)
	{
		var bmp0 = gtm.RenderString(s, Color.Black, Color.White, 50);
		bmp0?.Save($"{file.Name}-{s}.png", ImageFormat.Png);
	}
	
