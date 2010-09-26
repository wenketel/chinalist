// ==UserScript==
// @name Remove gougou ads
// @namespace http://code.google.com/p/adblock-chinalist/downloads/list
// @version 0.1
// @include http://web.gougou.com/search?*
// @description Remove gougou Ads for ChinaList
// @author:Gythialy
// @license GPL version 3 or any later version;
// http://www.gnu.org/copyleft/gpl.html
// ==/UserScript==
(function() {
	function remove(elm) {
		if (elm.snapshotItem) {
			for (var i = 0; i < elm.snapshotLength; i++) {
				remove(elm.snapshotItem(i));
			}
		} else if (elm[0]) {
			while (elm[0]) {
				remove(elm[0]);
			}
		} else {
			elm.parentNode.removeChild(elm);
		}
	}

	function x(xpath, parent, type, result) {
		return document.evaluate(xpath, parent || document, null, type || 7, result);
	}

	var links = x('//div[@class="ggResultItem"][a="推广"]');

	if (links.snapshotLength > 0) {
		remove(links);
	}
})();