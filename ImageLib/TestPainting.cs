using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ImageLib;
using NUnit.Framework;

namespace TestProject1 {
	[TestFixture]
	public class TestPainting : AssertionHelper {
		[Test]
		public void TestMethod1() {
			//var image = new CandidateImage(new[] { new ColoredRectangle(1, 1, 1, 1, new FastColor(128, 10, 20, 30)) });
			Expect(FastColor.Black.Blend(new FastColor(128, 255, 255, 255)), Is.EqualTo(FastColor.Black));
			Expect(new FastColor(128, 255, 255, 255).Blend(FastColor.Black), Is.EqualTo(new FastColor(255, 128, 128, 128)));
			Expect(new FastColor(128, 255, 255, 255).Blend(new FastColor(128, 255, 255, 255).Blend(FastColor.Black)), Is.EqualTo(new FastColor(255, 192, 192, 192)));
		}
	}
}
