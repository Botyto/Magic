require "Class"

Class.Node = {
	--main content
	title = false, --main title for this node
	summary = false, --summary of this node

	tags = false, --tags like 'private' or 'public' - used for file generation
}

function Node:ToString(writer, v)
	local ty = type(v)
	if ty == "table" then
		if not next(v) then
			return ""
		end
		local serialized = { }
		for i,entry in ipairs(v) do
			table.insert(serialized, self:ToString(writer, entry))
		end
		return table.concat(serialized, "\n")
	elseif ty == "string" then
		return v
	else
		return tostring(v)
	end
end

function Node:__ctor()
	self.tags = { private = true, public = true }
end

function Node:SetTag(tag, value)
	if tag == "public" and value then
		self.tags["public"] = true
		self.tags["private"] = true
	elseif tag == "private" and value then
		self.tags["public"] = nil
		self.tags["private"] = true
	else
		self.tags[tag] = value
	end
end

function Node:GenerateContent(writer)
	return ""
end

function Node:UpdateConnectionsWithNode(other)
end

function Node:TryUpdateConnectionMember(member, other)
	if self[member] == other.title then
		self[member] = other
		return true
	end
end

function Node:TryMergeNode(other)
end
