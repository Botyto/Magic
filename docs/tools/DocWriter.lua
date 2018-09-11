Class.DocWriter = {
	list_num = 0,
}

function DocWriter:ClearOutput(folder)
end

function DocWriter:Output(graph, folder)
	for i,node in ipairs(graph.nodes) do
		if IsKindOf(node, "PageNode") then
			local content = node:GenerateContent(self)
			print(node.file_path)
			print(content)
			--self:SaveFile(content, folder .. node.content)
		end
	end
end

function DocWriter:SaveFile(content, file)
	local f = io.open(file, "w")
	if f then
		f:write(content)
		f:close()
		print("Saved " .. file)
	else
		print("Failed to save " .. file)
	end
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
