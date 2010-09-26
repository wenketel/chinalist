// ==UserScript==
// @name Remove baidu ads
// @namespace http://code.google.com/p/adblock-chinalist/downloads/list
// @version 0.1
// @include http://www.baidu.com/s?*
// @include http://www.baidu.com/baidu?*
// @description Remove baidu Ads for ChinaList
// @copyright 2009+, chrisyue
// @license GPL version 3 or any later version;
// http://www.gnu.org/copyleft/gpl.html
// ==/UserScript==
/**
 * modified from baidu++
 * 
 * http://userscripts.org/scripts/show/47560
 */
(function() {
	function x(xpath, parent, type, result) {
		return document.evaluate(xpath, parent || document, null, type || 7,
				result);
	}

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

	var baidu = {

		ads : function() {
			return x("//table[@align='right']");
		},

		promotions : function() {
			return x("//a[text()='推广']/../../../../..");
		},

		sponsors : function() {
			return x("//td[starts-with(@id, 'taw')]/ancestor::table");
		},

		brands : function() {
			return x("//a[text()='品牌推广']/ancestor::table[1]");
		},

		body : function() {
			return document.body;
		},

		get : function(name, refresh) {
			var ret;
			var code = "\
      if (this._name == null || refresh) {\
        this._name = this.name();\
      }\
      ret = this._name;\
    ";
			eval(code.replace(/name/g, name));
			return ret;
		}
	};

	if ((cnt = baidu.get("brands").snapshotLength) > 0) {
		remove(baidu.get("brands"));
	}

	if ((cnt = baidu.get("sponsors").snapshotLength) > 0) {
		remove(baidu.get("sponsors"));
	}

	if ((cnt = baidu.get("promotions").snapshotLength) > 0) {
		remove(baidu.get("promotions"));
	}

	if ((cnt = baidu.get("ads").snapshotLength) > 0) {
		remove(baidu.get("ads"));
	}
})();
