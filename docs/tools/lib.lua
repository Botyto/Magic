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

function table.find(t, v)
	for key,value in pairs(t) do
		if v == value then
			return key
		end
	end
end

function table.insert_unique(t, v)
	if table.find(t, v) then
		return
	end

	table.insert(t, v)
end

function table.removed_duplicates(t)
	local keys = { }

	for i,v in ipairs(t) do
		keys[v] = true
	end

	local result = { }
	for v in pairs(keys) do
		table.insert(result, v)
	end

	return result
end

function string.gfind(str, pattern)
	local t = { }
	for entry in string.gmatch(str, pattern) do
		table.insert(t, entry)
	end

	return table.unpack(t)
end
