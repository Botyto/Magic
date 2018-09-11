require "Class"

Class.Node = {
	--main content
	title = false, --main title for this node
	summary = false, --summary of this node

	tags = false, --tags like 'private' or 'public' - used for file generation
}

function Node:ToString(v)
	local ty = type(v)
	if ty == "table" then
		if not next(v) then
			return ""
		end
		local serialized = { }
		for i,entry in ipairs(v) do
			table.insert(serialized, self:ToString(entry))
		end
		return table.concat(serialized, "\n")
	elseif ty == "string" then
		return v
	else
		return tostring(v)
	end
end

function Node:GenerateContent(writer)
	return ""
end
