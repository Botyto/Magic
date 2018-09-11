empty_table = { }

function string.trim_spaces(str)
   return str:gsub("^%s*(.-)%s*$", "%1")
end

function string.starts_with(str, query)
	return string.sub(str, 1, #query) == query
end

function string.split(str, separator)
	separator = separator or "%s"
	local pattern = "([^" .. separator .. "]+)"

	local result = { }
	for match in string.gmatch(str, pattern) do
		table.insert(result, match)
	end
	return result
end

function table.append(t1, t2) --appends t2 to end of t1
	for i,v in ipairs(t2) do
		table.insert(t1, v)
	end
end

function string.gfind(str, pattern)
	local t = { }
	for entry in string.gmatch(str, pattern) do
		table.insert(t, entry)
	end

	return table.unpack(t)
end
