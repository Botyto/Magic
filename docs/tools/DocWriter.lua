Class.DocWriter = {
	list_num = 0,
	required_tags = false,
}

function DocWriter:__ctor()
	self.required_tags = { }
end

function DocWriter:ClearOutput(folder)
	print("Clearing output folder: " .. folder)
end

function DocWriter:Output(graph, folder)
	local tag_names = { }
	for tag,required in pairs(self.required_tags) do
		table.insert(tag_names, required and tag or ("-" .. tag))
	end
	print("Output to folder: " .. folder .. " with tags: " .. table.concat(tag_names, ", "))

	for i,node in ipairs(graph.nodes) do
		if self:ShouldOutputNode(node) and IsKindOf(node, "PageNode") then
			local header = self:GenerateFileHeader(node)
			local content = node:GenerateContent(self)
			local footer = self:GenerateFileFooter(node)
			local page = string.format("%s\n%s\n%s", header, content, footer)
			self:SaveFile(page, folder .. node.file_path)
		end
	end
end

function DocWriter:ShouldOutputNode(node)
	for tag,required in pairs(self.required_tags) do
		if node.tags and node.tags[tag] ~= required then
			return false
		end
	end

	return true
end

function DocWriter:SaveFile(content, file)
	if string.find(file, "[%*%?\"',=+\\&^%$#@!{}%[%]<>%(%)]") then
		print("Failed to save " .. file)
		return
	end

	local f, err = io.open(file, "w")
	if f then
		f:write(content)
		f:close()
		print("Saved " .. file)
		return true
	else
		print("Failed to save " .. file .. ": " .. err)
	end
end

function DocWriter:GenerateFileHeader(node)
	return ""
end

function DocWriter:GenerateFileFooter(node)
	return ""
end

-------------------------------------------------

function DocWriter:GenerateList(list)
	--unordered list
	return table.concat(list, "\n")
end

function DocWriter:GenerateCaption(caption)
	--main page caption
	return caption
end

function DocWriter:GenerateSection(section)
	--page sections
	return section
end

function DocWriter:GenerateSnippet(code, language, description)
	--code snippet
	return code .. "\n" .. description
end

function DocWriter:GenerateTable(t, columns)
	--grid table
	local rows = { table.concat(columns, ",") }
	for i,row in ipairs(t) do
		table.insert(rows, table.concat(row, ","))
	end
	return table.concat(rows, "\n")
end

function DocWriter:GenerateLink(url, text)
	--link
	return text .. "(" .. url .. ")"
end

function DocWriter:ResetOrderedList()
	self.list_num = 0
end

function DocWriter:GenerateOrderedListLine(text)
	self.list_num = self.list_num + 1
	return text
end
