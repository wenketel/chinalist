// ==UserScript==
// @name Qiyi ads Remover
// @author Gythialy
// @description Remove qiyi.com flash ads for ChinaList
// @create 2011-6-16
// @lastmodified 2011-6-24
// @version 1.0.3
// @updateURL:https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_qiyi.user.js
// @namespace http://code.google.com/p/adblock-chinalist/
// @include http://www.qiyi.com/*
// @include http://yule.qiyi.com/*
// ==/UserScript==

(function() {
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
		for (var i = 0, length = ids.length; i < length; i++) {
			var t = document.getElementById(ids[i]);
			if (t)
				t.style.display = 'none';
		}
	}

	var url = window.location.href.toString();
	if (url.indexOf('www') > -1) {
		var cookies = ['qysyskin', 'qycrazyjm', 'qycrazy', 'qydyskin', 'qydycrazy', 'qydsjskin', 'qydsjcrazy'];
		for (var i = 0, length = cookies.length; i < length; i++) {
			var value = getQCookie(cookies[i]);
			value = value == null ? 0 : value;
			if (value < 5)
				setQCookie(cookies[i], 5, 1);
		}
	}

	var qyylcrazy = getQCookie('qyylcrazy');
	qyylcrazy = qyylcrazy == null ? 0 : qyylcrazy;
	if (qyylcrazy < 2 && url.indexOf('yule') > -1) {
		setQCookie('qyylcrazy', 2, 1);
	}

	var jdtflash = document.getElementById('jdtflash');
	if (jdtflash)
		jdtflash.style.display = '';

	var ids = ['adflash', 'backgroundskin', 'clicka'];
	hideAdByIds(ids);
})();