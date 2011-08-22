// ==UserScript==
// @name Qiyi ads Remover
// @namespace gythialy@chinalist
// @description Remove qiyi.com flash ads for ChinaList
// @author Gythialy
// @create 2011-6-16
// @lastmodified 2011-8-21
// @version 1.0.4
// @updateURL:https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_qiyi.user.js
// @include http://www.qiyi.com/*
// @include http://yule.qiyi.com/*
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

	function hideAdByIds(ids) {
		for ( var i = 0, length = ids.length; i < length; i++) {
			var t = document.getElementById(ids[i]);
			if (t) {
				t.style.display = 'none';
				log('hide ' + ids[i]);
			}
		}
	}

	var url = window.location.href.toString();
	if (url.indexOf('www') > -1) {
		var cookies = [ 'qyskin', ' qycrazysz', ' qycrazy', 'qydsjskin' ];
		for ( var i = 0, length = cookies.length; i < length; i++) {
			var value = getQCookie(cookies[i]);
			value = value == null ? 0 : value;
			log(cookies[i] + ' value is: ' + value);
			if (value < 5)
				setQCookie(cookies[i], 5, 1);
		}
	}

	var qyylcrazy = getQCookie('ylcrazyjm');
	qyylcrazy = qyylcrazy == null ? 0 : qyylcrazy;
	if (qyylcrazy < 2 && url.indexOf('yule') > -1) {
		setQCookie('ylcrazyjm', 2, 1);
	}

	var jdtflash = document.getElementById('jdtflash');
	if (jdtflash)
		jdtflash.style.display = '';

	var ids = [ 'adflash', 'backgroundskin', 'clicka', 'floatLayerFavDiv' ];
	hideAdByIds(ids);
})();