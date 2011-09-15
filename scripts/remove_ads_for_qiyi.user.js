// ==UserScript==
// @name Qiyi ads Remover
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.6
// @description Remove qiyi.com flash ads for ChinaList
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_qiyi.user.js
// @include http://*.qiyi.com/*
// ==/UserScript==

(function() {
	var DEBUG = 0;
	function log(message) {
		if (DEBUG && GM_log) {
			GM_log(message);
		}
	}

	function getQCookie(name) {
		var arr = document.cookie.match(new RegExp("(^| )" + name + "=([^;]*)(;|$)"));
		if (arr != null)
			return unescape(arr[2]);

		return null;
	}

	function setQCookie(name, value, expire) {
		var crazy_exp = new Date();
		crazy_exp.setTime(crazy_exp.getTime() + expire * 24 * 60 * 60 * 1000);
		document.cookie = name + "=" + value + ";expires=" + crazy_exp.toGMTString();
	}

	function hideAds(ids) {
		for ( var i = 0, length = ids.length; i < length; i++) {
			var t = document.getElementById(ids[i]);
			if (t) {
				t.style.display = 'none';
				log('hide ' + ids[i]);
			}
		}
	}

	if (document.getElementById('floatLayerFavDiv'))
		setQCookie('floatLayerFav', 1, 1);

	if (document.getElementById('jdtflash'))
		document.getElementById('jdtflash').style.display = '';

	var ids = [ 'adflash', 'backgroundskin', 'clicka', 'floatLayerFavDiv' ];
	hideAds(ids);
})();